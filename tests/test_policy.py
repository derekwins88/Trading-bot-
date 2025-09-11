from src.policy import DayPolicy

def test_day_policy_clamp():
    p = DayPolicy(max_trades=2, dd_limit_r=-1.0)
    assert p.can_enter()
    p.register(-0.6); assert p.can_enter()
    p.register(-0.5); assert not p.can_enter()  # cumR = -1.1 ≤ limit
