-- events.on("TileMapLoaded", function(mapId)
-- end)

for i = 1, 10 do
    local character_sprite = CurrentModulePath .. "/Assets/Sprites/Characters/purple_character.png"
    local character = entity.spawn("character", character_sprite, "Purple", 5 * i, 3, 5)
end

local foundMap = tilemap.load(CurrentModulePath, "Maps/map1.json")

-- log.error("ESTO ES UN ERROR!")

-- local character_sprite = CurrentModulePath .. "/Assets/Sprites/Characters/purple_character.png"
-- local character = entity.spawn("character", character_sprite, "Purple", 5, 3, 5)



-- character:set_stats(50, 5)
-- character:move(100,8)

-- if character ~= nil then
--     log("Character created! With " .. character.health .. " of HP")

--     camera.follow(character)
--     camera.set_smooth(0.15)
-- end

-- local character2_sprite = CurrentModulePath .. "/Assets/Sprites/Characters/yellow_character.png"
-- local character2 = entity.spawn("character", character2_sprite, "Yellow", 6, 10, 5)

-- character2:set_stats(50, 5)
-- character2:move(200,8)

