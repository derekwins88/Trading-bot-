from __future__ import annotations
import json, os
from typing import List, Dict


def load_trades(ndjson_path: str) -> List[Dict]:
    if not os.path.exists(ndjson_path): return []
    out = []
    with open(ndjson_path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if line:
                out.append(json.loads(line))
    return out


def compute_metrics(trades: List[Dict]) -> Dict:
    pnl = [t.get("pnl", 0.0) for t in trades]
    wins = sum(1 for x in pnl if x > 0)
    losses = sum(1 for x in pnl if x <= 0)
    eq = 0.0; peak = 0.0; maxdd = 0.0
    for x in pnl:
        eq += x
        peak = max(peak, eq)
        maxdd = min(maxdd, eq - peak)
    return {
        "trades": len(pnl),
        "wins": wins, "losses": losses,
        "win_rate": (wins / len(pnl)) if pnl else 0.0,
        "net_pnl": sum(pnl),
        "max_drawdown": maxdd
    }

