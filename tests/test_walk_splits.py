from src.walkforward import WFSpec, rolling_windows

def test_rolling_windows_count():
    spec = WFSpec(train_bars=0, test_bars=50, step_bars=25)
    wins = rolling_windows(200, spec)  # 200 bars total
    # windows: [0..50), [25..75), [50..100), [75..125), [100..150), [125..175)
    assert len(wins) == 6
    # each window's test span length == 50
    assert all((te - ts) == 50 for (_, _, ts, te) in wins)
