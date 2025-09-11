# -*- coding: utf-8 -*-
from __future__ import annotations
from typing import Literal, Tuple
import numpy as np

ExitReason = Literal["target", "stop", "time"]

def fill_trade(
    entry: float,
    side: str,
    stop: float,
    target: float,
    highs: np.ndarray,
    lows: np.ndarray,
    max_bars: int = 200,
    stop_first: bool = True,
) -> Tuple[float, ExitReason, int]:
    """
    Simulate bar-by-bar fills after entry using arrays of highs/lows
    that start on the *next* bar. Returns (exit_price, reason, bars_held).

    If both stop and target would hit on the same bar, `stop_first=True`
    chooses the conservative outcome.
    """
    assert side in ("long", "short")
    n = min(max_bars, highs.size)
    for i in range(n):
        hi, lo = float(highs[i]), float(lows[i])
        if side == "long":
            hit_stop = lo <= stop
            hit_target = hi >= target
        else:
            hit_stop = hi >= stop
            hit_target = lo <= target

        if stop_first:
            if hit_stop:
                return (stop, "stop", i + 1)
            if hit_target:
                return (target, "target", i + 1)
        else:
            if hit_target:
                return (target, "target", i + 1)
            if hit_stop:
                return (stop, "stop", i + 1)

    # Timeout: exit at last bar mid-price
    last_close = (float(highs[-1]) + float(lows[-1])) * 0.5 if n > 0 else entry
    return (last_close, "time", n)
