# -*- coding: utf-8 -*-
from __future__ import annotations
import argparse, json, os
import pandas as pd
from .risk import RiskParams
from .policy import DayPolicy
from .scanner import multi_entry_scan

def load_csv(path: str) -> pd.DataFrame:
    df = pd.read_csv(path)
    if "timestamp" in df.columns:
        df["timestamp"] = pd.to_datetime(df["timestamp"])
        df = df.set_index("timestamp")
    return df

def main():
    ap = argparse.ArgumentParser(description="A/B compare collapse vs recovery strategies")
    ap.add_argument("--csv", required=True)
    ap.add_argument("--symbol", default="ES")
    ap.add_argument("--atr", type=int, default=14)
    ap.add_argument("--lookahead", type=int, default=64)
    ap.add_argument("--cooldown", type=int, default=10)
    ap.add_argument("--max-trades", type=int, default=8)
    ap.add_argument("--dd-r", type=float, default=-5.0)
    ap.add_argument("--risk-pct", type=float, default=0.016)
    ap.add_argument("--rr", type=float, default=2.5)
    ap.add_argument("--atr-mult", type=float, default=1.5)
    ap.add_argument("--rev-k", type=float, default=1.0)
    ap.add_argument("--ma", type=int, default=20)
    args = ap.parse_args()

    df = load_csv(args.csv)
    outA = f"artifacts/{args.symbol}_A"
    outB = f"artifacts/{args.symbol}_B"
    risk = RiskParams(risk_pct=args.risk_pct, rr=args.rr, atr_mult=args.atr_mult)
    policy = DayPolicy(max_trades=args.max_trades, dd_limit_r=args.dd_r)

    tradesA, R_A = multi_entry_scan(df, args.symbol, risk, outdir=outA, atr_period=args.atr,
                                    look_ahead_bars=args.lookahead, cooldown_bars=args.cooldown,
                                    day_policy=policy, mode="collapse")
    tradesB, R_B = multi_entry_scan(df, args.symbol, risk, outdir=outB, atr_period=args.atr,
                                    look_ahead_bars=args.lookahead, cooldown_bars=args.cooldown,
                                    day_policy=policy, mode="recovery", rev_k=args.rev_k, ma_period=args.ma)

    os.makedirs("artifacts", exist_ok=True)
    with open("artifacts/ab_summary.json", "w") as f:
        json.dump({"symbol": args.symbol,
                   "A_collapse": {"trades": tradesA, "cumR": R_A},
                   "B_recovery": {"trades": tradesB, "cumR": R_B}}, f, indent=2)
    print(f"[A/B] collapse: trades={tradesA} cumR={R_A:.2f} | recovery: trades={tradesB} cumR={R_B:.2f}")

if __name__ == "__main__":
    main()
