Vector3 pos = targetTransform.position;
Matrix4x4 vp = mainCamera.projectionMatrix * mainCamera.worldToCameraMatrix;
Vector4 porjPos = vp * new Vector4(pos.x, pos.y, pos.z, 1);
// projPos.xyz / projPos.w [-1, 1] in view frustum
