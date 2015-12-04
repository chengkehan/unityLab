LuaAssets = class(MonoObject)

function LuaAssets:ctor()

end

function LuaAssets:Init()

	return self
end

function LuaAssets:LoadAssets_EscapePods(completeCallback)
	AssetsLoader.GetInstance():LoadAssets("EscapePods_assets", completeCallback)
end

function LuaAssets:NewAssets(assetsData, assetsName)
	return AssetsLoader.GetInstance():InstantiateAssets(assetsData, assetsName)
end

function LuaAssets:GetAssets(assetsData, assetsName)
	return AssetsLoader.GetInstance():FetchAssets(assetsData, assetsName)
end

function LuaAssets:GetAssets_EscapePods_InitAnimation(assetsData)
	return self:GetAssets(assetsData, "InitAnimation")
end