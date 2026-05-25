import base64, os

b64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAn0lEQVR4nO3XsQ3CMABFwZgx2H821jAdog2S9bB818f5ekoRXxcAAHCisfoFc87X52VjPFc986vHysN3IEA9oCZAPaAmQD2gJkA9oCZAPaB2+y7w/Z/+r+7cH47/Ao4P4Dq88vAdCFAPqAlQD6gJUA+oCVAPqLkMrRyyg+MDuAytPHwHAtQDagLUA2oC1ANqAtQDagLUA2rHBwAAgEO9AUE3IE5DMwE5AAAAAElFTkSuQmCC"

out_path = os.path.join(os.path.dirname(__file__), "..", "Textures", "UI", "ToggleGridOverlay.png")
os.makedirs(os.path.dirname(out_path), exist_ok=True)
with open(out_path, "wb") as f:
    f.write(base64.b64decode(b64))
print(f"Wrote {out_path}")
