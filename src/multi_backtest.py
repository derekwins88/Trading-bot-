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
    ap = argparse.ArgumentParser()
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
    args = ap.parse_args()

    df = load_csv(args.csv)
    trades, cumR = multi_entry_scan(
        df=df,
        symbol=args.symbol,
        risk=RiskParams(risk_pct=args.risk_pct, rr=args.rr, atr_mult=args.atr_mult),
        atr_period=args.atr,
        look_ahead_bars=args.lookahead,
        cooldown_bars=args.cooldown,
        day_policy=DayPolicy(max_trades=args.max_trades, dd_limit_r=args.dd_r),
    )
    os.makedirs("artifacts", exist_ok=True)
    with open("artifacts/session_summary.json", "w") as f:
        json.dump({"symbol": args.symbol, "trades": trades, "cumR": cumR}, f, indent=2)
    print(f"[OK] {args.symbol} trades={trades} cumR={cumR:.2f}")

if __name__ == "__main__":
    main()
