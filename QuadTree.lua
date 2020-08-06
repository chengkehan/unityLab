QuadTree = {}
QuadTree.__index = QuadTree

QuadTree.MAX_DEPTH = 5

function QuadTree:new(_x, _y, _w, _h, _depth)
    local o = {}
    setmetatable(o, QuadTree)

    if _depth == nil then
        _depth = 1
    end

    o.x = _x
    o.y = _y
    o.w = _w
    o.h = _h
    o.depth = _depth
    o.children = nil
    o.objects = nil
    o.objects_qtree_mapping = nil

    if _depth == 1 then
        o.objects_qtree_mapping = {}
    end

    if _depth >= QuadTree.MAX_DEPTH then
        o.objects = {}
    else
        local x = _x
        local y = _y
        local w = math.floor(_w / 2)
        local h = math.floor(_h / 2)
        local depth = _depth + 1
        o.children = {
            QuadTree:new(x, y, w, h),
            QuadTree:new(x + w, y, w, h),
            QuadTree:new(x, y + h, w, h),
            QuadTree:new(x + w, y + h, w, h)
        }
    end

    return o
end

function QuadTree:Destroy()
    util.clear_table(self.objects)
    self.objects = nil

    if self.objects_qtree_mapping ~= nil then
        util.clear_table(self.objects_qtree_mapping)
        self.objects_qtree_mapping = nil
    end
    
    if self.children ~= nil then
        for k, v in pairs(self.children) do
            local child = v
            child:Destroy()
            self.children[k] = nil
        end
        self.children = nil
    end
end

function QuadTree:Add(obj, x, y)
    if self:Contains(obj) then
        return false
    end

    return self:_Add(obj, x, y, self)
end

function QuadTree:Remove(obj)
    if self:Contains(obj) == false then
        return false
    end

    local qtree = self.objects_qtree_mapping[obj]
    qtree.objects[obj] = nil
    self.objects_qtree_mapping[obj] = nil
end

function QuadTree:Update(obj, x, y)
    if self:Contains(obj) == false then
        return false
    end

    local qtree = self.objects_qtree_mapping[obj]
    if qtree:IsInArea(x, y) == false then
        self:Remove(obj)
        self:Add(obj, x, y)
    end
end

function QuadTree:Contains(obj)
    if self.objects_qtree_mapping == nil then
        return false
    end
    return self.objects_qtree_mapping[obj] ~= nil
end

function QuadTree:IsInArea(x, y)
    return x >= self.x and x < self.x + self.w and y >= self.y and y < self.y + self.h
end

function QuadTree:IsIntersectedWith(x, y, w, h)
    local oleft = x
    local otop = y
    local oright = oleft + w
    local obottom = otop + h
    local left = self.x
    local top = self.y
    local right = left + self.w
    local bottom = top + self.h

    if oright < left or obottom < top or oleft > right or otop > bottom then
        return false
    else
        return true
    end
end

function QuadTree:CollisionWithRectangle(x, y, w, h, o_resultList)
    if self:IsIntersectedWith(x, y, w, h) then
        if self.children ~= nil then
            for childI = 1, #self.children do
                local child = self.children[childI]
                child:CollisionWithRectangle(x, y, w, h, o_resultList)
            end
        else
            for i = 1, #self.objects do
                table.insert(o_resultList, self.objects[i])
            end
        end
    end
end

function QuadTree:_Add(obj, x, y, rootQTree)
    if self:IsInArea(x, y) then
        if self.children == nil then
            self.objects[obj] = obj
            rootQTree.objects_qtree_mapping[obj] = self
            return true
        else
            for childI = 1, #self.children do
                local child = self.children[childI]
                if child:_Add(obj, x, y, rootQTree) then
                    return true
                end
            end
        end
    end
    return false
end
