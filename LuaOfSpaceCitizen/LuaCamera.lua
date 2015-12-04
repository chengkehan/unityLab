LuaCamera = class(MonoObject)

function LuaCamera:ctor()

end

function LuaCamera:dtor()
	
end

function LuaCamera:Init()
	self.gameObject = GameObject("MainCamera")
	self.transform = self.gameObject.transform
	self.camera = self.gameObject:AddComponent("Camera")
	self:SetDepth(0)
	self:SetClearFlag(true)
	self:SetAsMainCamera()

	return self
end

function LuaCamera:GetGameObject()
	return self.gameObject
end

function LuaCamera:GetTransform()
	return self.transform
end

function LuaCamera:GetCamera()
	return self.camera
end

function LuaCamera:GetDepth()
	if self.camera ~= nil then
		return self.camera.depth
	end
	return 0
end

function LuaCamera:SetDepth(depth)
	if self.camera ~= nil then
		self.camera.depth = depth
	end
end

function LuaCamera:SetClearFlag(blackOrSkybox)
	if self.camera ~= nil then
		if black then
			self.camera.clearFlags = CameraClearFlags.SolidColor
			self.camera.backgroundColor = Color.black
		else
			self.camera.clearFlags = CameraClearFlags.Skybox
		end
	end
end

function LuaCamera:SetAsMainCamera()
	if self.gameObject ~= nil then
		self.gameObject.tag = "MainCamera"
	end
end