LuaMath = class(MonoObject)

function LuaMath:ctor()

end

function LuaMath:Init()

	return self
end

function LuaMath:Vector3_Subtraction(value1, value2)
	return Vector3(value1.x - value2.x, value1.y - value2.y, value1.z - value2.z)
end

function LuaMath:Vector3_Addition(value1, value2)
	return Vector3(value1.x + value2.x, value1.y + value2.y, value1.z + value2.z)
end

function LuaMath:Quaternion_Multiply(lhs, rhs)
	return Quaternion(lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y, lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z, lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x, lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z)
end