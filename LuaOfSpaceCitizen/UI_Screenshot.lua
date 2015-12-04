UI_Screenshot = class(MonoObject)

function UI_Screenshot:ctor()

end

function UI_Screenshot:dtor()
	global:Mono():DestroyLuaObject(self.cameraSnapshop)
end

function UI_Screenshot:Init(completeCallback)
	self.completeCallback = completeCallback
	global:UI():OpenUI_Screenshot(LuaMemberMethod(self, self.ScreenshotCompleteCallback))

	return self
end

function UI_Screenshot:ScreenshotCompleteCallback(ui)
	global:Event():Register_OnDestroyGameObject(ui, LuaMemberMethod(self, self.dtor))

	self.cameraSnapshop = CameraSnapshot.new():Init(global:Camera():MainCamera())

	self.uiTexture = global:UI():NGUI_Find(ui, "AnchorCenter/Texture"):GetComponent("UITexture")
	self.uiTexture.mainTexture = self.cameraSnapshop:Render();

	self.uiSlider = global:UI():NGUI_Find(ui, "AnchorCenter/ProgressBar"):GetComponent("UISlider")

	self.completeCallback:Call()
end

function UI_Screenshot:SetProgress(progress)
	if self.uiSlider ~= nil then
		self.uiSlider.value = progress
	end
end

function UI_Screenshot:Close()
	global:UI():CloseUI_Screenshot()
end
