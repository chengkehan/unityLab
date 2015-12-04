UI_GameStart = class(MonoObject)

function UI_GameStart:ctor()

end

function UI_GameStart:Init()
	global:UI():OpenUI_GameStart(LuaMemberMethod(self, self.GameStartCompleteCallback))

	return self
end

function UI_GameStart:GameStartCompleteCallback(ui)
	global:UI():NGUI_OnClick(ui, "AnchorCenter/Button", LuaMemberMethod(self, self.StartButtonOnClick))
end

function UI_GameStart:StartButtonOnClick()
	global:UI():CloseUI_GameStart()
	Scene_EscapePods.new():Init()
end