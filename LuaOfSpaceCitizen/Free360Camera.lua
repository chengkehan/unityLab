Free360Camera = class(MonoObject)

function Free360Camera:ctor()
	
end

function Free360Camera:Init()
	global:Camera():CreateMainCamera()
	self.mainCameraTransform = global:Camera():MainCamera():GetTransform()
	self.mainCameraLookAround_mousePosition = 0
	self.mainCameraLookAround_isWorking = false
	self.mainCameraLookAround_isTweening = false
	self.mainCameraLookAround_tweenSpeed = 100
	self.mainCameraLookAround_eulerAnglesX = 0
	self.mainCameraLookAround_eulerAnglesY = 0
	self.mainCameraLookAround_eulerAnglesZ = 0

	return self
end

function Free360Camera:LookAround()
	if self.mainCameraTransform ~= nil then
		if Input.GetMouseButtonDown(0) then
			self.mainCameraLookAround_mousePosition = Input.mousePosition
			self.mainCameraLookAround_isWorking = true
			self.mainCameraLookAround_isTweening = false
		end
		if Input.GetMouseButtonUp(0) then
			self.mainCameraLookAround_isWorking = false
			self.mainCameraLookAround_isTweening = true
		end
		if self.mainCameraLookAround_isWorking then
			local mousePosition = Input.mousePosition
			local offset = global:Math():Vector3_Subtraction(mousePosition, self.mainCameraLookAround_mousePosition)
			self.mainCameraLookAround_eulerAnglesX = 0
			self.mainCameraLookAround_eulerAnglesY = 0
			self.mainCameraLookAround_eulerAnglesZ = 0
			local rotateSpeed = 0.2
			if offset.x < 0 then
				self.mainCameraLookAround_eulerAnglesY = -rotateSpeed * Time.deltaTime * offset.x
			elseif offset.x > 0 then
				self.mainCameraLookAround_eulerAnglesY = -rotateSpeed * Time.deltaTime * offset.x
			end
			if offset.y < 0 then
				self.mainCameraLookAround_eulerAnglesX = rotateSpeed * Time.deltaTime * offset.y
			elseif offset.y > 0 then
				self.mainCameraLookAround_eulerAnglesX = rotateSpeed * Time.deltaTime * offset.y
			end
			self.mainCameraTransform.rotation = global:Math():Quaternion_Multiply(self.mainCameraTransform.rotation, Quaternion.Euler(self.mainCameraLookAround_eulerAnglesX, self.mainCameraLookAround_eulerAnglesY, self.mainCameraLookAround_eulerAnglesZ))
		end
		if self.mainCameraLookAround_isTweening then
			self.mainCameraLookAround_eulerAnglesX = self.mainCameraLookAround_eulerAnglesX * Mathf.Min(self.mainCameraLookAround_tweenSpeed * Time.deltaTime, 0.9)
			self.mainCameraLookAround_eulerAnglesY = self.mainCameraLookAround_eulerAnglesY * Mathf.Min(self.mainCameraLookAround_tweenSpeed * Time.deltaTime, 0.9)
			self.mainCameraLookAround_eulerAnglesZ = self.mainCameraLookAround_eulerAnglesZ * Mathf.Min(self.mainCameraLookAround_tweenSpeed * Time.deltaTime, 0.9)
			self.mainCameraTransform.rotation = global:Math():Quaternion_Multiply(self.mainCameraTransform.rotation, Quaternion.Euler(self.mainCameraLookAround_eulerAnglesX, self.mainCameraLookAround_eulerAnglesY, self.mainCameraLookAround_eulerAnglesZ))
			if Mathf.Abs(self.mainCameraLookAround_eulerAnglesX) < 0.0001 and Mathf.Abs(self.mainCameraLookAround_eulerAnglesY) < 0.0001 and Mathf.Abs(self.mainCameraLookAround_eulerAnglesZ) < 0.0001 then
				self.mainCameraLookAround_isTweening = false
			end
		end
	end
end