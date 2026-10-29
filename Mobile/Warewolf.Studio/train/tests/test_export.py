import json

from transformers import BertConfig, BertModel, BertTokenizer

from system1.export import export, max_logit_difference
from system1.model import System1Classifier

VOCAB = ["[PAD]", "[UNK]", "[CLS]", "[SEP]", "[MASK]", "goal", ":", "assign", "decision", "end", "steps", "so", "far", "##s"]


def tiny(tmp_path):
    """A random two-layer BERT with a 14-word vocabulary: no download, no client data."""
    vocab = tmp_path / "vocab.txt"
    vocab.write_text("\n".join(VOCAB))
    tokenizer = BertTokenizer(str(vocab))
    encoder = BertModel(BertConfig(vocab_size=len(VOCAB), hidden_size=32, num_hidden_layers=2, num_attention_heads=2, intermediate_size=64))
    return tokenizer, System1Classifier(encoder, n_labels=3)


def test_onnx_logits_match_torch_and_the_kotlin_side_gets_what_it_needs(tmp_path):
    tokenizer, model = tiny(tmp_path)
    out = tmp_path / "model"

    onnx_path = export(model, tokenizer, ["Assign", "Decision", "END"], out, max_len=16, meta={"mode": "frozen"})
    batch = tokenizer(["goal : assign", "steps so far : decision decisions end"], padding=True, return_tensors="pt")

    assert max_logit_difference(model, onnx_path, batch["input_ids"], batch["attention_mask"]) < 1e-4
    assert max_logit_difference(model, out / "model.int8.onnx", batch["input_ids"], batch["attention_mask"]) < 0.5
    assert (out / "vocab.txt").read_text().split("\n")[:len(VOCAB)] == VOCAB
    assert json.loads((out / "labels.json").read_text()) == ["Assign", "Decision", "END"]
    config = json.loads((out / "config.json").read_text())
    assert config["maxLength"] == 16 and config["truncation"] == "left" and config["lowerCase"] is True
