    for i = 1, 10 do
        local character_sprite = CurrentModulePath .. "/Assets/Sprites/Characters/character_yellow_idle.png"
        local character = entity.spawn("character", character_sprite, "Yellow", 5 * (i*0.5), 15, 5)
    end

