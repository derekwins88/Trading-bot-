import numpy as np
from src.execution import fill_trade


def test_stop_first_when_both_hit_long():
    entry, stop, target = 100.0, 99.0, 102.0
    highs = np.array([102.5])     # target hit
    lows  = np.array([98.5])      # stop hit same bar
    exit_px, reason, bars = fill_trade(entry, "long", stop, target, highs, lows, stop_first=True)
    assert reason == "stop" and exit_px == stop and bars == 1


def test_target_first_option_short():
    entry, stop, target = 100.0, 102.0, 98.0
    highs = np.array([103.0])     # stop hit
    lows  = np.array([97.0])      # target hit same bar
    exit_px, reason, bars = fill_trade(entry, "short", stop, target, highs, lows, stop_first=False)
    assert reason == "target" and exit_px == target and bars == 1
