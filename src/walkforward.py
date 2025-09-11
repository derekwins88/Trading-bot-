# -*- coding: utf-8 -*-
from __future__ import annotations
from dataclasses import dataclass
from typing import List, Tuple, Dict
import os, json
import pandas as pd
from .risk import RiskParams
from .policy import DayPolicy
from .scanner import multi_entry_scan


@dataclass
class WFSpec:
    train_bars: int = 0      # reserved for future tuning; ignored for now
    test_bars: int = 500     # OOS length per split
    step_bars: int = 250     # how far to advance between splits

def rolling_windows(n_bars: int, spec: WFSpec) -> List[Tuple[int,int,int,int]]:
    """
    Produce (train_start, train_end, test_start, test_end) index tuples.
    train_* are placeholders (0..train_end), test_* is the OOS slice we evaluate.
    """
    wins: List[Tuple[int,int,int,int]] = []
    t, k, s = spec.train_bars, spec.test_bars, spec.step_bars
    start = 0
    while True:
        train_start = max(0, start)
        train_end   = max(0, start + t)           # currently not used
        test_start  = train_end
        test_end    = min(n_bars, test_start + k)
        if test_end - test_start < k:             # not enough bars to evaluate
            break
        wins.append((train_start, train_end, test_start, test_end))
        start = test_start + s
        if start + t + k >= n_bars:
            break
    return wins

def evaluate_walkforward(
    df: pd.DataFrame,
    symbol: str,
    outdir: str,
    wf: WFSpec,
    mode: str,                      # "collapse" or "recovery"
    risk: RiskParams,
    look_ahead_bars: int = 64,
    cooldown_bars: int = 10,
    day_policy: DayPolicy = DayPolicy(),
    rev_k: float = 1.0,
    ma_period: int = 20,
) -> Dict:
    os.makedirs(outdir, exist_ok=True)
    splits = rolling_windows(len(df), wf)
    results = []
    net_R = 0.0
    total_trades = 0

    for i, (tr_s, tr_e, te_s, te_e) in enumerate(splits, start=1):
        # NOTE: we only evaluate on test window [te_s:te_e]
        dfi = df.iloc[te_s:te_e].copy()
        sym_out = os.path.join(outdir, f"split_{i:02d}")
        os.makedirs(sym_out, exist_ok=True)
        trades, cumR = multi_entry_scan(
            df=dfi,
            symbol=symbol,
            risk=risk,
            outdir=sym_out,
            atr_period=14,
            look_ahead_bars=look_ahead_bars,
            cooldown_bars=cooldown_bars,
            day_policy=DayPolicy(max_trades=day_policy.max_trades, dd_limit_r=day_policy.dd_limit_r),
            mode="collapse" if mode == "collapse" else "recovery",
            rev_k=rev_k,
            ma_period=ma_period,
        )
        total_trades += trades
        net_R += cumR
        results.append({"split": i, "bars": int(te_e - te_s), "trades": trades, "cumR": cumR})

    summary = {"mode": mode, "splits": len(splits), "total_trades": total_trades, "net_R": net_R, "by_split": results}
    with open(os.path.join(outdir, "wf_summary.json"), "w") as f:
        json.dump(summary, f, indent=2)
    return summary
