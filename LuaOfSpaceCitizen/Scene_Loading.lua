Scene_Loading = class(MonoObject)

function Scene_Loading:ctor()

end

function Scene_Loading:dtor()
	global:Mono():RemoveUpdate(LuaMemberMethod(self, self.Free360CameraUpdate))
end

function Scene_Loading:Init()
	global:Scene():LoadScene_Loading(LuaMemberMethod(self, self.LoadSceneCompleteCallback), nil)
	global:Mono():AddUpdate(LuaMemberMethod(self, self.Free360CameraUpdate))
	UI_GameStart.new():Init();

	return self
end

function Scene_Loading:LoadSceneCompleteCallback()
	self.camera = Free360Camera.new():Init()
	global:Event():Register_OnDestroyScene(LuaMemberMethod(self, self.dtor))
end

function Scene_Loading:Free360CameraUpdate()
	if self.camera then
		self.camera:LookAround()
	end
end