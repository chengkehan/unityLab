// Convert ugui anchored position to world position without RectTransform
private Vector3 AnchoredPositionToWorldPosition(Vector2 anchoredPosition)
{
    Vector2 canvasSize = canvasRectTransform.sizeDelta;

    Vector2 viewportPosition = new Vector2(
        (anchoredPosition.x + (canvasSize.x * 0.5f)) / canvasSize.x,
        (anchoredPosition.y + (canvasSize.y * 0.5f)) / canvasSize.y
    );
    Vector3 worldPosition = CameraManager.GetInstance().GetUICamera().ViewportToWorldPoint(viewportPosition);
    worldPosition.z = canvasRectTransform.position.z;

    return worldPosition;
}
