LuaUI = class(MonoObject)

function LuaUI:ctor()

end

function LuaUI:Init()

	return self
end

function LuaUI:OpenUI_GameStart(completeCallback)
	UIManager.GetInstance():ShowUI("GameStart", completeCallback)
end

function LuaUI:CloseUI_GameStart()
	UIManager.GetInstance():HideUI("GameStart")
end

function LuaUI:OpenUI_Screenshot(completeCallback)
	UIManager.GetInstance():ShowUI("Screenshot", completeCallback)
end

function LuaUI:CloseUI_Screenshot()
	UIManager.GetInstance():HideUI("Screenshot")
end

function LuaUI:NGUI_Find(ui, component)
	return ui.transform:FindChild(component).gameObject
end

function LuaUI:NGUI_OnClick(ui, component, onClick)
	UIEventListener.Get(ui.transform:FindChild(component).gameObject).onClick_lua = onClick
end