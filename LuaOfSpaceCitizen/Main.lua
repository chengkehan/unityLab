Main = class(MonoObject)

function Main:ctor()

end

function Main:Init()
	Scene_Loading.new():Init();

	return self
end