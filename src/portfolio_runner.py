# -*- coding: utf-8 -*-
from __future__ import annotations
import argparse, json, os
import pandas as pd
from typing import Dict
from .risk import RiskParams
from .policy import DayPolicy
from .scanner import multi_entry_scan
from .metrics import load_trades, compute_metrics


def load_csv(path: str) -> pd.DataFrame:
    df = pd.read_csv(path)
    if "timestamp" in df.columns:
        df["timestamp"] = pd.to_datetime(df["timestamp"])
        df = df.set_index("timestamp")
    return df


def parse_symbol_map(pairs: list[str]) -> Dict[str, str]:
    out: Dict[str, str] = {}
    for p in pairs:
        if ":" not in p:
            raise ValueError(f"--csv expects SYMBOL:path.csv, got {p}")
        sym, path = p.split(":", 1)
        out[sym.strip()] = path.strip()
    return out


def main():
    ap = argparse.ArgumentParser(description="Portfolio multi-entry backtest")
    ap.add_argument("--csv", action="append", required=True,
                    help="SYMBOL:path.csv (repeatable). Example: --csv ES:data/es.csv --csv NQ:data/nq.csv")
    ap.add_argument("--atr", type=int, default=14)
    ap.add_argument("--lookahead", type=int, default=64)
    ap.add_argument("--cooldown", type=int, default=10)
    ap.add_argument("--max-trades", type=int, default=8)
    ap.add_argument("--dd-r", type=float, default=-5.0)
    ap.add_argument("--risk-pct", type=float, default=0.016)
    ap.add_argument("--rr", type=float, default=2.5)
    ap.add_argument("--atr-mult", type=float, default=1.5)
    args = ap.parse_args()

    symmap = parse_symbol_map(args.csv)
    risk = RiskParams(risk_pct=args.risk_pct, rr=args.rr, atr_mult=args.atr_mult)
    policy = DayPolicy(max_trades=args.max_trades, dd_limit_r=args.dd_r)

    os.makedirs("artifacts", exist_ok=True)
    portfolio = {}
    net_R = 0.0
    total_trades = 0

    for sym, path in symmap.items():
        df = load_csv(path)
        outdir = f"artifacts/{sym}"
        trades, cumR = multi_entry_scan(
            df=df,
            symbol=sym,
            risk=risk,
            outdir=outdir,                # per-symbol capsules
            atr_period=args.atr,
            look_ahead_bars=args.lookahead,
            cooldown_bars=args.cooldown,
            day_policy=policy,
        )
        # metrics per symbol
        m = compute_metrics(load_trades(f"{outdir}/trades.ndjson"))
        portfolio[sym] = {"trades": trades, "cumR": cumR, "metrics": m}
        net_R += cumR
        total_trades += trades

    summary = {"symbols": list(symmap.keys()), "total_trades": total_trades, "net_R": net_R, "by_symbol": portfolio}
    with open("artifacts/portfolio_summary.json", "w") as f:
        json.dump(summary, f, indent=2)
    print(f"[OK] Portfolio symbols={list(symmap.keys())} total_trades={total_trades} net_R={net_R:.2f}")


if __name__ == "__main__":
    main()

