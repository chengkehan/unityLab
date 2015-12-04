LuaCameraManager = class(MonoObject)

function LuaCameraManager:ctor()

end

function LuaCameraManager:Init()
	self.cameras = {}

	return self
end

function LuaCameraManager:CreateMainCamera()
	local camera = LuaCamera.new():Init()
	self.cameras[LUA_CAMERA_MAIN] = camera
end

function LuaCameraManager:Camera(cameraID)
	return self.cameras[cameraID]
end

function LuaCameraManager:MainCamera()
	return self.cameras[LUA_CAMERA_MAIN]
end