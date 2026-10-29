"""ONNX export of a trained classifier, with what the Kotlin side needs to tokenize and read its output."""
import json
from pathlib import Path

import numpy as np
import onnx
import onnxruntime as ort
import torch
from onnxruntime.quantization import QuantType, quantize_dynamic

from .model import System1Classifier


def export(model: System1Classifier, tokenizer, labels: list[str], out_dir: Path, max_len: int, meta: dict) -> Path:
    """
    Writes `model.onnx` (float), `model.int8.onnx` (dynamic int8), `vocab.txt`, `labels.json` and `config.json`.
    Inputs are `input_ids` and `attention_mask` (int64, batch x sequence); the output is `logits`.
    """
    out_dir.mkdir(parents=True, exist_ok=True)
    model = model.cpu().eval()
    # Two texts of different lengths: torch.export fixes any dimension it only ever sees as 1.
    dummy = tokenizer(["Goal: x", "Goal: a longer goal to pad against"], return_tensors="pt", padding=True)
    batch, sequence = torch.export.Dim("batch"), torch.export.Dim("sequence", max=max(max_len, 8))
    float_path = out_dir / "model.onnx"
    torch.onnx.export(
        model,
        (dummy["input_ids"], dummy["attention_mask"]),
        str(float_path),
        input_names=["input_ids", "attention_mask"],
        output_names=["logits"],
        dynamic_shapes={"input_ids": {0: batch, 1: sequence}, "attention_mask": {0: batch, 1: sequence}},
        opset_version=18,
        dynamo=True,
        external_data=False,
    )
    # The exporter's intermediate shape annotations trip the quantizer's shape inference; it recomputes them.
    exported = onnx.load(str(float_path))
    del exported.graph.value_info[:]
    onnx.save(exported, str(float_path))
    quantize_dynamic(str(float_path), str(out_dir / "model.int8.onnx"), weight_type=QuantType.QInt8)
    # WordPiece vocabulary, one token per line in id order, as the Kotlin tokenizer reads it.
    vocab = sorted(tokenizer.get_vocab().items(), key=lambda item: item[1])
    assert [i for _, i in vocab] == list(range(len(vocab))), "token ids must be contiguous"
    (out_dir / "vocab.txt").write_text("\n".join(token for token, _ in vocab), encoding="utf-8")
    (out_dir / "labels.json").write_text(json.dumps(labels), encoding="utf-8")
    config = {
        "maxLength": max_len,
        "lowerCase": bool(getattr(tokenizer, "do_lower_case", True)),
        # Over-long texts lose their start (the goal), never the most recent steps at the end.
        "truncation": "left",
        **meta,
    }
    (out_dir / "config.json").write_text(json.dumps(config, indent=2), encoding="utf-8")
    return float_path


def max_logit_difference(model: System1Classifier, onnx_path: Path, input_ids: torch.Tensor, attention_mask: torch.Tensor) -> float:
    """How far the ONNX model's logits are from torch's on the same batch."""
    model = model.cpu().eval()
    with torch.no_grad():
        expected = model(input_ids, attention_mask).numpy()
    session = ort.InferenceSession(str(onnx_path), providers=["CPUExecutionProvider"])
    actual = session.run(["logits"], {"input_ids": input_ids.numpy().astype(np.int64), "attention_mask": attention_mask.numpy().astype(np.int64)})[0]
    return float(np.abs(expected - actual).max())
