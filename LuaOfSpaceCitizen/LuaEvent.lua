LuaEvent = class(MonoObject)

function LuaEvent:ctor()

end

function LuaEvent:Init()

	return self
end

function LuaEvent:Register_OnDestroyScene(action)
	NotificationCenter.GetInstance():Register(NC_MSG_DESTROY_SCENE, action, 1)
end

function LuaEvent:Register_OnDestroyGameObject(target, action)
	LuaMonoBehaviour.Get(target).onDestroy = action
end