inputs.enable_map("Player")

character_sprite = CurrentModulePath .. "/Assets/Sprites/Characters/purple_character.png"
character = entity.spawn("character", character_sprite, "Purple", 5, 3, 5)

game_loop.check_update(function(delta)
    local direction = inputs.get_vector2("Player", "Move")
    character:move(direction.x, direction.y)
end)

local foundMap = tilemap.load(CurrentModulePath, "Maps/map1.json")

character:set_stats(50, 5)

if character ~= nil then
    debug.log("Character created! With " .. character.health .. " of HP")

    camera.follow(character)
    camera.set_smooth(0.15)
end

local character2_sprite = CurrentModulePath .. "/Assets/Sprites/Characters/yellow_character.png"
local character2 = entity.spawn("character", character2_sprite, "Yellow", 6, 10, 5)

character2:set_stats(50, 5)