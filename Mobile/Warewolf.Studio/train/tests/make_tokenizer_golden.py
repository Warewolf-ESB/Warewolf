"""
Writes the Kotlin WordPieceTokenizer's golden fixture: token ids Hugging Face's fast BertTokenizer (the one
train.py uses) gives for synthetic sentences over a tiny vocabulary. Rerun after changing the cases:

    train/.venv/Scripts/python train/tests/make_tokenizer_golden.py
"""
import json
import tempfile
from pathlib import Path

from transformers import BertTokenizerFast

VOCAB = [
    "[PAD]", "[UNK]", "[CLS]", "[SEP]", "[MASK]",
    "goal", ":", "steps", "so", "far", "on", "this", "branch", "-", ">", "(", ")", "[", "]", ",", ".", "'", '"', "*",
    "assign", "set", "default", "##s", "name", "rs", "f", "decision", "is", "valid", "?", "the", "true", "arm", "of",
    "previous", "step", "none", "cafe", "earlier", "2", "un", "##know", "##n", "word", "中", "文",
]

CASES = [
    "Goal: Set defaults",
    "Steps so far on this branch: Assign \"Set defaults\" [[name]], [[rs(*).f]] -> Decision \"Is valid?\"",
    "This branch is the 'True' arm of the previous step.",
    "Steps so far on this branch: (none)",
    "(2 earlier steps) -> Assign",
    "CAFÉ café Café",
    "unknown words: zzz",
    "tabs\tand\nnew lines\r\nand  double  spaces",
    "中文 mixed中文",
    "a" * 120,
    "control\u0000char​s",
]
MAX_LENGTH = 12

with tempfile.TemporaryDirectory() as tmp:
    vocab = Path(tmp) / "vocab.txt"
    vocab.write_text("\n".join(VOCAB), encoding="utf-8")
    tokenizer = BertTokenizerFast(str(vocab), do_lower_case=True)
    full = [tokenizer(text)["input_ids"] for text in CASES]
    tokenizer.truncation_side = "left"
    truncated = [tokenizer(text, truncation=True, max_length=MAX_LENGTH)["input_ids"] for text in CASES]

out = Path(__file__).resolve().parents[2] / "shared/generator/src/test/resources/wordpiece-golden.json"
out.write_text(json.dumps({
    "vocab": VOCAB,
    "maxLength": MAX_LENGTH,
    "cases": [{"text": t, "ids": f, "truncatedIds": tr} for t, f, tr in zip(CASES, full, truncated)],
}, ensure_ascii=False, indent=1), encoding="utf-8")
print(f"wrote {len(CASES)} cases to {out}")
