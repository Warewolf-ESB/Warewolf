# System 1 classifier training

Trains the System 1 next-step classifier (`docs/Plan.md` §2, option a) and exports it to ONNX for
`ClassifierChooser` (`shared/generator`). Training data and models are derived from client workflows and never
enter the repo; they live beside the corpus (`C:\Users\ultra\wwc\` on the author's machine).

## Setup

```
python -m venv --system-site-packages train/.venv
train/.venv/Scripts/python -m pip install -r train/requirements.txt
```

Blackwell (sm_120) GPUs also need the CUDA 12.8 torch build; see the comment in `requirements.txt`.

## Run

1. Export features from the Kotlin harness. Each row's text is `ChoiceContext.describeState`, which the phone builds the same way:
   `gradlew :eval:run --args="<derivedDir> --export-features <featuresDir>"`
2. Train and export:
   `train/.venv/Scripts/python train/train.py --features <featuresDir> --out <modelDir> [--encoder BAAI/bge-small-en-v1.5] [--mode frozen|finetune]`
3. Score it through the Kotlin chooser:
   `gradlew :eval:run --args="<derivedDir> --client classifier --model-dir <modelDir> [--float]"`

`<modelDir>` holds `model.onnx`, `model.int8.onnx`, `vocab.txt`, `labels.json`, `config.json` (token window, left
truncation, recent steps, results) and `predictions-python-eval.jsonl`. A Kotlin `--float` run should agree with
that file on every row. `finetune` carries more of the client corpus into the weights; it is for internal use only
until shipping it is cleared.

On Git Bash, set `MSYS_NO_PATHCONV=1` and pass Windows-style paths to `gradlew`.

## Tests

`train/.venv/Scripts/python -m pytest train/tests`. `tests/make_tokenizer_golden.py` regenerates the Kotlin
tokenizer's golden fixture.
