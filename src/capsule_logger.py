import json, os, time
from typing import Dict, Any

def ensure_dir(path: str) -> None:
    os.makedirs(path, exist_ok=True)

def write_ndjson(path: str, obj: Dict[str, Any]) -> None:
    ensure_dir(os.path.dirname(path))
    with open(path, "a", encoding="utf-8") as f:
        f.write(json.dumps(obj, separators=(",", ":")) + "\n")

def trade_capsule(symbol: str, side: str, entry_px: float, exit_px: float,
                  verdict: Dict[str, Any], t0: str, t1: str) -> dict:
    pnl = (exit_px - entry_px) * (1 if side == "long" else -1)
    return {
        "capsule_id": f"TRADE⇌{int(time.time())}",
        "symbol": symbol,
        "side": side,
        "entry": entry_px,
        "exit": exit_px,
        "pnl": pnl,
        "verdict": verdict,
        "t0": t0,
        "t1": t1
    }
