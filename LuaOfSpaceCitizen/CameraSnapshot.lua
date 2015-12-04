CameraSnapshot = class(MonoObject)

function CameraSnapshot:ctor()

end

function CameraSnapshot:dtor()
	global:Mono():DestroyUnityObject(self.renderTexture)
	global:Mono():DestroyUnityObject(self.newCameraGo)
end

function CameraSnapshot:Init(camera)
	self.camera = camera

	return self
end

function CameraSnapshot:Render()
	if self.camera ~= nil and self.camera:GetGameObject() ~= nil then
		if self.newCameraGo == nil then
			self.newCameraGo = GameObject("CameraSnapshot")
			self.newCamera = self.newCameraGo:AddComponent("Camera")
			self.newCamera.clearFlags = self.camera:GetCamera().clearFlags
			self.newCamera.backgroundColor = self.camera:GetCamera().backgroundColor
			self.newCamera.cullingMask = self.camera:GetCamera().cullingMask
			self.newCamera.orthographic = self.camera:GetCamera().orthographic
			self.newCamera.fieldOfView = self.camera:GetCamera().fieldOfView
			self.newCamera.nearClipPlane = self.camera:GetCamera().nearClipPlane
			self.newCamera.farClipPlane = self.camera:GetCamera().farClipPlane
			self.newCamera.rect = self.camera:GetCamera().rect
			self.newCamera.depth = self.camera:GetCamera().depth;
			self.newCamera.enabled = false
		end
		self.newCameraGo.transform.parent = self.camera:GetTransform().parent
		self.newCameraGo.transform.localPosition = self.camera:GetTransform().localPosition
		self.newCameraGo.transform.rotation = self.camera:GetTransform().rotation

		if self.renderTexture == nil then
			self.renderTexture = RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.ARGB32)
		end
		self.newCamera.targetTexture = self.renderTexture
		self.newCamera:Render()

		return self.renderTexture
	end

	return nil
end