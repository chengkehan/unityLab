LuaScene = class(MonoObject)

function LuaScene:ctor()

end

function LuaScene:Init()

	return self
end

function LuaScene:LoadScene_Loading(completeCallback, progressCallback)
	SceneManager.GetInstance():LoadScene("Loading_scene", "Loading", completeCallback, progressCallback)
end

function LuaScene:LoadScene_EscapePods(completeCallback, progressCallback)
	SceneManager.GetInstance():LoadScene("EscapePods_scene", "plant", completeCallback, progressCallback)
end