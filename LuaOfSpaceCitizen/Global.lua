Global = class(MonoObject)

function Global:ctor()

end

function Global:Init()
	self.util = Util.new()

	self.luaCamera = LuaCameraManager.new():Init()

	self.scene = LuaScene.new():Init()

	self.assets = LuaAssets.new():Init()

	self.ui = LuaUI.new():Init()

	self.mono = LuaMono.new():Init()

	self.event = LuaEvent.new():Init()

	self.math = LuaMath.new():Init()

	Main.new():Init()

	return self
end

function Global:Util()
	return self.util
end

function Global:Camera()
	return self.luaCamera
end 

function Global:Scene()
	return self.scene
end

function Global:Assets()
	return self.assets
end

function Global:UI()
	return self.ui
end

function Global:Mono()
	return self.mono
end

function Global:Event()
	return self.event
end

function Global:Math()
	return self.math
end