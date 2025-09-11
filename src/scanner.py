from __future__ import annotations
import pandas as pd
import numpy as np
from typing import Iterable, Tuple
from .entropy_engine import atr, delta_phi, verdict_from_series
from .risk import RiskParams, position_size, stops_targets
from .session import session_weight
from .execution import fill_trade
from .capsule_logger import trade_capsule, write_ndjson
from .policy import DailyBook, DayPolicy
from .strategies import entry_side, Mode
def stream_dphi(high: np.ndarray, low: np.ndarray, close: np.ndarray, atr_period: int) -> np.ndarray:
    a = atr(high, low, close, atr_period)
    return delta_phi(a, close)

def multi_entry_scan(
    df: pd.DataFrame,
    symbol: str,
    risk: RiskParams,
    outdir: str = "artifacts",
    atr_period: int = 14,
    look_ahead_bars: int = 64,
    cooldown_bars: int = 10,
    day_policy: DayPolicy = DayPolicy(),
    equity: float = 50_000.0,
    mode: Mode = "collapse",
    rev_k: float = 1.0,
    ma_period: int = 20,
) -> Tuple[int, float]:
    """
    Walks the chart; on each bar i, compute verdict from a rolling window (up to i),
    fire on collapse (⟿), then simulate bar-by-bar fills forward.
    Returns (num_trades, cumR).
    """
    assert isinstance(df.index, pd.DatetimeIndex), "df index must be DatetimeIndex"
    high, low, close = df["high"].values, df["low"].values, df["close"].values
    dphi_all = stream_dphi(high, low, close, atr_period)
    book = DailyBook(day_policy)

    trades = 0
    cumR = 0.0
    cooldown = 0

    warmup = max(atr_period + 20, 30)
    for i in range(warmup, len(close) - 2):
        ts = df.index[i]
        day_key = ts.date().isoformat()
        policy = book.policy_for(day_key)

        if cooldown > 0:
            cooldown -= 1
            continue
        if not policy.can_enter():
            continue

        # verdict from series up to i (rolling)
        v = verdict_from_series(dphi_all[: i + 1])

        # Gate by glyph according to mode
        if mode == "collapse":
            if v.glyph != "⟿":
                continue
        else:  # recovery mode
            if v.glyph != "☑":
                continue

        atr_i = float(atr(high, low, close, atr_period)[i])  # ATR at i
        side = entry_side(mode, close[: i + 1], atr_i, lookback=20, k=rev_k, ma_period=ma_period)
        if side == "wait":
            continue

        entry = float(close[i])
        w = session_weight(ts)
        size = max(1, int(position_size(equity, atr_i, entry, risk) * w))
        stop, target = stops_targets(entry, side, atr_i, risk)

        highs_next = high[i + 1 : i + 1 + look_ahead_bars]
        lows_next  = low [i + 1 : i + 1 + look_ahead_bars]
        exit_px, reason, bars_held = fill_trade(entry, side, stop, target, highs_next, lows_next)

        risk_per_unit = abs(entry - stop)
        r_mult = ((exit_px - entry) if side == "long" else (entry - exit_px)) / risk_per_unit if risk_per_unit > 0 else 0.0
        cumR += r_mult
        trades += 1
        policy.register(r_mult)
        cooldown = cooldown_bars

        cap = trade_capsule(
            symbol, side, entry, exit_px,
            {
                "glyph": v.glyph, "np_wall": v.np_wall, "no_recovery": v.no_recovery,
                "sat_like": v.sat_like, "ΔΦ_last": v.delta_phi, "size": size,
                "stop": stop, "target": target, "exit_reason": reason, "bars_held": bars_held, "R": r_mult
            },
            str(ts), str(df.index[min(len(df.index) - 1, i + 1 + bars_held)])
        )
        write_ndjson(f"{outdir}/trades.ndjson", cap)

    return trades, cumR
