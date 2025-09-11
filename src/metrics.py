from __future__ import annotations
import json, os
from typing import List, Dict


def load_trades(ndjson_path: str) -> List[Dict]:
    if not os.path.exists(ndjson_path):
        return []
    out = []
    with open(ndjson_path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if line:
                out.append(json.loads(line))
    return out


def compute_metrics(trades: List[Dict]) -> Dict:
    """Supports either PnL-based or R-multiple-based capsules."""
    pnl = []
    rvals = []
    for t in trades:
        if "pnl" in t:
            pnl.append(float(t.get("pnl", 0.0)))
        v = t.get("verdict", {})
        if isinstance(v, dict) and "R" in v:
            try:
                rvals.append(float(v["R"]))
            except Exception:
                pass

    # Equity curve for maxDD
    eq = 0.0
    peak = 0.0
    maxdd = 0.0
    for x in pnl:
        eq += x
        peak = max(peak, eq)
        maxdd = min(maxdd, eq - peak)

    wins = sum(1 for x in rvals if x > 0)
    losses = sum(1 for x in rvals if x <= 0)
    win_rate = (wins / len(rvals)) if rvals else 0.0
    expectancy_R = (sum(rvals) / len(rvals)) if rvals else 0.0

    return {
        "count": len(trades),
        "pnl_sum": sum(pnl) if pnl else 0.0,
        "max_drawdown": maxdd,
        "wins": wins,
        "losses": losses,
        "win_rate": win_rate,
        "cum_R": sum(rvals) if rvals else 0.0,
        "avg_R": expectancy_R,
    }

