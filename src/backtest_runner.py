import argparse, json, os
import pandas as pd
from .strategy import EntropyStrategy, Params
from .metrics import load_trades, compute_metrics

def load_csv(path: str) -> pd.DataFrame:
    df = pd.read_csv(path)
    # expect columns: timestamp, open, high, low, close, volume
    if "timestamp" in df.columns:
        df["timestamp"] = pd.to_datetime(df["timestamp"])
        df = df.set_index("timestamp")
    return df

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--csv", required=True)
    ap.add_argument("--symbol", default="ES")
    ap.add_argument("--hud", action="store_true")
    ap.add_argument("--report", action="store_true")
    args = ap.parse_args()

    df = load_csv(args.csv)
    bot = EntropyStrategy(args.symbol, Params(), outdir="artifacts")
    res = bot.run(df, hud=args.hud)

    if args.report:
        os.makedirs("artifacts", exist_ok=True)
        with open("artifacts/report.json", "w") as f:
            json.dump(res, f, indent=2)
        mets = compute_metrics(load_trades("artifacts/trades.ndjson"))
        with open("artifacts/metrics.json", "w") as f:
            json.dump(mets, f, indent=2)

if __name__ == "__main__":
    main()
