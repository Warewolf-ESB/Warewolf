import json

import pytest
import torch

from system1.data import class_weights, encode_labels, load_labels, load_rows


def test_rows_and_labels_load_from_the_exported_files(tmp_path):
    (tmp_path / "labels.json").write_text(json.dumps(["Assign", "Decision", "END"]))
    (tmp_path / "train.jsonl").write_text('{"text":"Goal: a","label":"END"}\n\n{"text":"Goal: b","label":"Assign"}\n')

    assert load_labels(tmp_path) == ["Assign", "Decision", "END"]
    assert [r["label"] for r in load_rows(tmp_path / "train.jsonl")] == ["END", "Assign"]


def test_labels_encode_to_their_position_in_labels_json():
    rows = [{"label": "END"}, {"label": "Assign"}]

    assert encode_labels(rows, ["Assign", "Decision", "END"]) == [2, 0]


def test_a_label_missing_from_labels_json_is_rejected():
    with pytest.raises(ValueError, match="Sort Records"):
        encode_labels([{"label": "Sort Records"}], ["Assign", "END"])


def test_rarer_classes_weigh_more_and_unseen_classes_weigh_nothing():
    weights = class_weights([0, 0, 0, 0, 1], n_labels=3)

    assert weights[1] > weights[0] > 0
    assert weights[2] == 0
    assert torch.isclose(weights[:2].mean(), torch.tensor(1.0))
