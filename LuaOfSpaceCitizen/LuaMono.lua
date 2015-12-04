LuaMono = class(MonoObject)

function LuaMono:ctor()

end

function LuaMono:Init()

	return self
end

function LuaMono:CreateGameObject(name)
	return GameObject(name)
end

function LuaMono:AddUpdate(action)
	GlobalLoop.GetInstance():AddUpdate(action)
end

function LuaMono:RemoveUpdate(action)
	GlobalLoop.GetInstance():RemoveUpdate(action)
end

function LuaMono:ExecuteAtEndOfFrame(action)
	GlobalLoop.GetInstance():StartCoroutine_WaitForEndOfFrame(action)
end

function LuaMono:AddComponent(target, componentName)
	if target ~= nil and componentName ~= nil then
		return target:AddComponent(componentName)
	end
	return nil
end

function LuaMono:RemoveComponent(target, componentName)
	local component = self:GetComponent(target, componentName)
	self:DestroyUnityObject(component)
end

function LuaMono:GetComponent(target, componentName)
	if target ~= nil and componentName ~= nil then
		return target:GetComponent(componentName)
	end
	return nil
end

function LuaMono:DestroyUnityObject(object)
	if object ~= nil then
		Object.Destroy(object)
	end
end

function LuaMono:DestroyLuaObject(object)
	if object ~= nil then
		object:dtor()
	end
end