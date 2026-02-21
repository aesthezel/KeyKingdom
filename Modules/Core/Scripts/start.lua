inputs.enable_map("Player")

character_sprite = CurrentModulePath .. "/Assets/Sprites/Characters/red_character.png"
character = entity.spawn("character", character_sprite, "Personaje Rojo", 5.5, 3.25, 5)

character:set_stats(100, 200)

game_loop.check_update(function(delta)
    local direction = inputs.get_vector2("Player", "Move")
    character:move(direction.x, direction.y)
end)

if character ~= nil then
    debug.log("Character created! With " .. character.health .. " of HP")

    camera.follow(character)
    camera.set_smooth(0.15)
end

local foundMap = tilemap.load(CurrentModulePath, "Maps/map1.json")