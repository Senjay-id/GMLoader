function __SplatError() //gml_Script___SplatError
{
    var _string = ""
    var _i = 0
    repeat argument_count
    {
        _string += string(argument[_i])
        _i++
    }
    //show_error(("Splat:\n" + _string + "\n "), true)
}

