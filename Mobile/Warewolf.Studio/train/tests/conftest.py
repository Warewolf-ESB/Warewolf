import sys
from pathlib import Path

# Lets the tests import `system1` the way train.py does, run from any directory.
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
