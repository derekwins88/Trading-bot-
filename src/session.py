from __future__ import annotations
from dataclasses import dataclass
import pandas as pd


@dataclass
class SessionWeights:
    asia: float = 0.6
    london: float = 1.0
    ny: float = 1.5


def session_weight(ts: pd.Timestamp, w: SessionWeights = SessionWeights()) -> float:
    # Simple UTC bands (tweak to your feed TZ):
    h = ts.hour
    if 0 <= h < 8:     # Asia
        return w.asia
    if 8 <= h < 13:    # London
        return w.london
    return w.ny       # NY default

