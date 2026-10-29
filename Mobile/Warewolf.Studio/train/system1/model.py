import torch
from torch import nn


class System1Classifier(nn.Module):
    """A sentence encoder, mean-pooled, then a linear layer over the tools and END. Outputs raw logits."""

    def __init__(self, encoder: nn.Module, n_labels: int, dropout: float = 0.1):
        super().__init__()
        self.encoder = encoder
        self.dropout = nn.Dropout(dropout)
        self.head = nn.Linear(encoder.config.hidden_size, n_labels)

    def embed(self, input_ids: torch.Tensor, attention_mask: torch.Tensor) -> torch.Tensor:
        hidden = self.encoder(input_ids=input_ids, attention_mask=attention_mask).last_hidden_state
        mask = attention_mask.unsqueeze(-1).to(hidden.dtype)
        return (hidden * mask).sum(1) / mask.sum(1).clamp(min=1e-9)

    def forward(self, input_ids: torch.Tensor, attention_mask: torch.Tensor) -> torch.Tensor:
        return self.head(self.dropout(self.embed(input_ids, attention_mask)))
