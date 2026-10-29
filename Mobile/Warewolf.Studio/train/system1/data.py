"""Reads the features the Kotlin harness exports (`gradlew :eval:run --args="<derived> --export-features <dir>"`)."""
import json
from collections import Counter
from pathlib import Path

import torch


def load_rows(path: Path) -> list[dict]:
    with open(path, encoding="utf-8") as f:
        return [json.loads(line) for line in f if line.strip()]


def load_labels(features_dir: Path) -> list[str]:
    """The classifier's outputs in order: every placeable tool in catalog order, then END."""
    return json.loads((features_dir / "labels.json").read_text(encoding="utf-8"))


def encode_labels(rows: list[dict], labels: list[str]) -> list[int]:
    index = {name: i for i, name in enumerate(labels)}
    unknown = sorted({r["label"] for r in rows} - index.keys())
    if unknown:
        raise ValueError(f"labels not in labels.json: {unknown}")
    return [index[r["label"]] for r in rows]


def class_weights(label_ids: list[int], n_labels: int, power: float = 0.5) -> torch.Tensor:
    """
    Inverse frequency to the given power, scaled so seen classes average 1. Rare tools get more weight without
    swamping END and Assign; a class never seen in training gets 0, since there is nothing to learn it from.
    """
    counts = Counter(label_ids)
    weights = torch.zeros(n_labels)
    for label, count in counts.items():
        weights[label] = count ** -power
    seen = weights > 0
    weights[seen] /= weights[seen].mean()
    return weights
