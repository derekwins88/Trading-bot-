# ΔΦ / verdict computation with CPU-only NumPy; CI-friendly
from __future__ import annotations
import numpy as np
from dataclasses import dataclass

NP_WALL = 0.09
RECOV_EPS = 0.045
RECOV_WIN = 8  # bars

@dataclass
class Verdict:
    np_wall: bool
    no_recovery: bool
    sat_like: bool
    glyph: str   # "⟿", "⚖", "☑"
    delta_phi: float

def atr(high: np.ndarray, low: np.ndarray, close: np.ndarray, period: int = 14) -> np.ndarray:
    prev_close = np.r_[close[0], close[:-1]]
    tr = np.maximum.reduce([high - low, np.abs(high - prev_close), np.abs(low - prev_close)])
    period = min(period, tr.size)
    kernel = np.ones(period) / period
    atr_val = np.convolve(tr, kernel, mode='same')
    atr_val[:period] = np.maximum.accumulate(atr_val[:period])
    return atr_val[:tr.size]

def delta_phi(atr_vals: np.ndarray, close: np.ndarray) -> np.ndarray:
    # normalize ATR by close to approximate “entropy” scale
    with np.errstate(divide='ignore', invalid='ignore'):
        dphi = np.clip(atr_vals / np.maximum(1e-9, close), 0.0, 1.0)
    return dphi

def verdict_from_series(dphi: np.ndarray) -> Verdict:
    if dphi.size == 0:
        return Verdict(False, False, True, "⚖", 0.0)
    np_wall = bool(np.any(dphi > NP_WALL))
    idx = np.where(dphi > NP_WALL)[0][-1] if np_wall else -1  # last spike

    recovered = False
    if np_wall:
        tail = dphi[idx + 1 : idx + 1 + RECOV_WIN]
        recovered = tail.size > 0 and np.all(tail <= RECOV_EPS)

    no_recovery = not recovered
    # tolerate tiny floating point drift when checking for non-increasing series
    sat_like = bool(np.all(np.diff(dphi) <= 1e-9))

    glyph = "⟿" if (np_wall and no_recovery and not sat_like) else ("⚖" if sat_like else "☑")
    return Verdict(np_wall, no_recovery, sat_like, glyph, float(dphi[-1]))
