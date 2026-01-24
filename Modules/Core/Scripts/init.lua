local MODULE_PATH = CurrentModulePath
local character_sprite = CurrentModulePath .. "/Assets/Sprites/Characters/yellow_character.png"
local character = entity.spawn("character", character_sprite, "Yellow", 5, 3, 5)

character:set_stats(50, 5)
character:move(300,8)

if character ~= nil then
    log("Character created! With " .. character.health .. " of HP")

    camera.follow(character)
    camera.set_smooth(0.15)
end

local foundMap = tilemap.load(CurrentModulePath, "Maps/map1.json")