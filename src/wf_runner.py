# -*- coding: utf-8 -*-
from __future__ import annotations
import argparse
import pandas as pd
from .walkforward import WFSpec, evaluate_walkforward
from .risk import RiskParams
from .policy import DayPolicy


def load_csv(path: str) -> pd.DataFrame:
    df = pd.read_csv(path)
    if "timestamp" in df.columns:
        df["timestamp"] = pd.to_datetime(df["timestamp"])
        df = df.set_index("timestamp")
    return df


def main():
    ap = argparse.ArgumentParser(description="Walk-forward evaluator")
    ap.add_argument("--csv", required=True)
    ap.add_argument("--symbol", default="ES")
    ap.add_argument("--mode", choices=["collapse", "recovery"], default="collapse")
    ap.add_argument("--test-bars", type=int, default=500)
    ap.add_argument("--step-bars", type=int, default=250)
    ap.add_argument("--max-trades", type=int, default=8)
    ap.add_argument("--dd-r", type=float, default=-5.0)
    ap.add_argument("--risk-pct", type=float, default=0.016)
    ap.add_argument("--rr", type=float, default=2.5)
    ap.add_argument("--atr-mult", type=float, default=1.5)
    ap.add_argument("--lookahead", type=int, default=64)
    ap.add_argument("--cooldown", type=int, default=10)
    ap.add_argument("--rev-k", type=float, default=1.0)
    ap.add_argument("--ma", type=int, default=20)
    args = ap.parse_args()

    df = load_csv(args.csv)
    spec = WFSpec(train_bars=0, test_bars=args.test_bars, step_bars=args.step_bars)

    risk = RiskParams(risk_pct=args.risk_pct, rr=args.rr, atr_mult=args.atr_mult)
    dayp = DayPolicy(max_trades=args.max_trades, dd_limit_r=args.dd_r)

    out = evaluate_walkforward(
        df=df, symbol=args.symbol, outdir="artifacts/wf",
        wf=spec, mode=args.mode, risk=risk,
        look_ahead_bars=args.lookahead, cooldown_bars=args.cooldown,
        day_policy=dayp, rev_k=args.rev_k, ma_period=args.ma
    )
    print(f"[WF] splits={out['splits']} total_trades={out['total_trades']} net_R={out['net_R']:.2f}")


if __name__ == "__main__":
    main()
