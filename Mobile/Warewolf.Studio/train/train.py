"""
Trains the System 1 classifier (Plan.md §2, option a) on the exported features and exports it to ONNX.

    train/.venv/Scripts/python train/train.py --features <featuresDir> --out <modelDir>
        [--encoder BAAI/bge-small-en-v1.5] [--mode frozen|finetune] [--epochs N] [--max-len 384]

`frozen` trains only the output layer on top of the pretrained encoder; `finetune` trains the whole encoder too,
which carries more of the client corpus into the weights (internal use only until shipping it is cleared).
The model is chosen on `val.jsonl`; `eval.jsonl` (the held-out sets) is scored once, at the end.
"""
import argparse
import json
import random
import sys
from pathlib import Path

import numpy as np
import torch
from torch import nn
from transformers import AutoModel, AutoTokenizer

from system1.data import class_weights, encode_labels, load_labels, load_rows
from system1.export import export, max_logit_difference
from system1.model import System1Classifier

END = "END"


def metrics(logits: torch.Tensor, gold: torch.Tensor, end_index: int) -> dict:
    """The harness's numbers (overall, END, tool picks) plus top-3 accuracy and how well P(END) separates ENDs."""
    predicted = logits.argmax(-1)
    correct = predicted == gold
    is_end = gold == end_index
    top3 = (logits.topk(3, -1).indices == gold.unsqueeze(-1)).any(-1)
    p_end = logits.softmax(-1)[:, end_index]
    pos, neg = p_end[is_end], p_end[~is_end]
    # Mann-Whitney AUC: the chance a real END scores above a continuation.
    auc = ((pos.unsqueeze(1) > neg.unsqueeze(0)).float().mean() + 0.5 * (pos.unsqueeze(1) == neg.unsqueeze(0)).float().mean()).item()
    return {
        "overall": correct.float().mean().item(),
        "end": correct[is_end].float().mean().item(),
        "tools": correct[~is_end].float().mean().item(),
        "top3": top3.float().mean().item(),
        "endAuc": auc,
    }


def describe(name: str, m: dict) -> str:
    return f"{name}: overall {m['overall']:.1%}  END {m['end']:.1%}  tools {m['tools']:.1%}  top-3 {m['top3']:.1%}  END AUC {m['endAuc']:.3f}"


def tokenize(tokenizer, rows: list[dict], max_len: int) -> dict:
    return tokenizer([r["text"] for r in rows], truncation=True, max_length=max_len, padding=True, return_tensors="pt")


def batches(n: int, size: int, shuffle: bool):
    order = torch.randperm(n) if shuffle else torch.arange(n)
    for start in range(0, n, size):
        yield order[start:start + size]


@torch.no_grad()
def predict(model: System1Classifier, encoded: dict, device: str, size: int = 128) -> torch.Tensor:
    model.eval()
    out = []
    for idx in batches(encoded["input_ids"].shape[0], size, shuffle=False):
        out.append(model(encoded["input_ids"][idx].to(device), encoded["attention_mask"][idx].to(device)).float().cpu())
    return torch.cat(out)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--features", type=Path, required=True)
    parser.add_argument("--out", type=Path, required=True)
    parser.add_argument("--encoder", default="BAAI/bge-small-en-v1.5")
    parser.add_argument("--mode", choices=["frozen", "finetune"], default="frozen")
    parser.add_argument("--epochs", type=int)
    parser.add_argument("--max-len", type=int, default=384)
    parser.add_argument("--batch", type=int, default=32)
    parser.add_argument("--seed", type=int, default=13)
    args = parser.parse_args()
    # The ONNX exporter prints emoji, which a Windows console's default code page cannot encode.
    sys.stdout.reconfigure(encoding="utf-8")

    random.seed(args.seed)
    np.random.seed(args.seed)
    torch.manual_seed(args.seed)
    device = "cuda" if torch.cuda.is_available() else "cpu"

    labels = load_labels(args.features)
    end_index = labels.index(END)
    splits = {name: load_rows(args.features / f"{name}.jsonl") for name in ("train", "val", "eval")}
    gold = {name: torch.tensor(encode_labels(rows, labels)) for name, rows in splits.items()}

    tokenizer = AutoTokenizer.from_pretrained(args.encoder)
    tokenizer.truncation_side = "left"
    encoded = {name: tokenize(tokenizer, rows, args.max_len) for name, rows in splits.items()}
    model = System1Classifier(AutoModel.from_pretrained(args.encoder), len(labels)).to(device)
    loss_fn = nn.CrossEntropyLoss(weight=class_weights(gold["train"].tolist(), len(labels)).to(device))

    if args.mode == "frozen":
        for p in model.encoder.parameters():
            p.requires_grad = False
        epochs = args.epochs or 60
        optimizer = torch.optim.AdamW(model.head.parameters(), lr=1e-3, weight_decay=0.01)
        # The encoder never changes, so embed every text once and train the output layer on the embeddings.
        with torch.no_grad():
            model.eval()
            embedded = {}
            for name, enc in encoded.items():
                parts = [model.embed(enc["input_ids"][i].to(device), enc["attention_mask"][i].to(device)) for i in batches(enc["input_ids"].shape[0], 128, False)]
                embedded[name] = torch.cat(parts)
        head_logits = lambda name: model.head(embedded[name]).float().cpu()
    else:
        epochs = args.epochs or 6
        optimizer = torch.optim.AdamW([
            {"params": model.encoder.parameters(), "lr": 3e-5},
            {"params": model.head.parameters(), "lr": 1e-3},
        ], weight_decay=0.01)

    best, best_state = -1.0, None
    for epoch in range(1, epochs + 1):
        model.train()
        n = gold["train"].shape[0]
        for idx in batches(n, args.batch if args.mode == "finetune" else 256, shuffle=True):
            y = gold["train"][idx].to(device)
            if args.mode == "frozen":
                logits = model.head(model.dropout(embedded["train"][idx]))
            else:
                logits = model(encoded["train"]["input_ids"][idx].to(device), encoded["train"]["attention_mask"][idx].to(device))
            loss = loss_fn(logits, y)
            optimizer.zero_grad()
            loss.backward()
            optimizer.step()

        model.eval()
        with torch.no_grad():
            val_logits = head_logits("val") if args.mode == "frozen" else predict(model, encoded["val"], device)
        m = metrics(val_logits, gold["val"], end_index)
        if m["overall"] > best:
            best, best_state = m["overall"], {k: v.detach().cpu().clone() for k, v in model.state_dict().items()}
            print(f"epoch {epoch:3d}  {describe('val', m)}  (best)")
        elif epoch % 10 == 0 or args.mode == "finetune":
            print(f"epoch {epoch:3d}  {describe('val', m)}")

    model.load_state_dict(best_state)
    model.eval()
    results = {name: metrics(predict(model, encoded[name], device), gold[name], end_index) for name in ("val", "eval")}
    print(describe("best val", results["val"]))
    print(describe("held-out eval", results["eval"]))

    # features.json says how the Kotlin side built each text (recentSteps); the chooser must build it the same way.
    features = json.loads((args.features / "features.json").read_text(encoding="utf-8"))
    meta = {**features, "encoder": args.encoder, "mode": args.mode, "labels": len(labels), "results": results}
    onnx_path = export(model, tokenizer, labels, args.out, args.max_len, meta)
    sample = {k: v[:16] for k, v in encoded["eval"].items()}
    for path in (onnx_path, args.out / "model.int8.onnx"):
        print(f"{path.name}: {path.stat().st_size / 1e6:.1f} MB, max logit difference from torch {max_logit_difference(model, path, sample['input_ids'], sample['attention_mask']):.4f}")

    # Python's own eval predictions, so the Kotlin chooser's run can be checked against them row by row.
    eval_logits = predict(model.to(device), encoded["eval"], device)
    with open(args.out / "predictions-python-eval.jsonl", "w", encoding="utf-8") as f:
        for row, p in zip(splits["eval"], eval_logits.argmax(-1).tolist()):
            f.write(json.dumps({"id": row["id"], "gold": row["label"], "predicted": labels[p]}) + "\n")


if __name__ == "__main__":
    main()
