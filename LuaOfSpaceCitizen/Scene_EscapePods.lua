Scene_EscapePods = class(MonoObject)

function Scene_EscapePods:ctor()

end

function Scene_EscapePods:Init()
	self.uiScreenshot = UI_Screenshot.new():Init(LuaMemberMethod(self, self.UIScreenshotInitCompleteCallback))
	
	return self
end

function Scene_EscapePods:UIScreenshotInitCompleteCallback()
	global:Scene():LoadScene_EscapePods(LuaMemberMethod(self, self.LoadSceneCompleteCallback), LuaMemberMethod(self, self.LoadSceneProgressCallback))
end

function Scene_EscapePods:LoadSceneCompleteCallback()
	global:Camera():CreateMainCamera()
	global:Assets():LoadAssets_EscapePods(LuaMemberMethod(self, self.LoadSceneAssetsCompleteCallback))
end

function Scene_EscapePods:LoadSceneProgressCallback(progress)
	self.uiScreenshot:SetProgress(progress * 0.5)
end

function Scene_EscapePods:LoadSceneAssetsCompleteCallback(assetsData)
	self:InitScene(assetsData)
	self.uiScreenshot:SetProgress(1.0)
	self.uiScreenshot:Close()
end

function Scene_EscapePods:InitScene(assetsData)
	local animatorController = global:Assets():GetAssets_EscapePods_InitAnimation(assetsData)
	self.animator = global:Mono():AddComponent(global:Camera():MainCamera():GetGameObject(), "Animator")
	self.animator.runtimeAnimatorController = animatorController
	global:Mono():AddUpdate(LuaMemberMethod(self, self.CheckAnimatorIsFinished))
end

function Scene_EscapePods:CheckAnimatorIsFinished()
	local animatorStateInfo = self.animator:GetCurrentAnimatorStateInfo(0);
	if animatorStateInfo.normalizedTime > 1 then
		print("Is Finished")
		global:Mono():RemoveUpdate(LuaMemberMethod(self, self.CheckAnimatorIsFinished))
		global:Mono():RemoveComponent(global:Camera():MainCamera():GetGameObject(), "Animator")
	end
end