function showMenu() {
        var menu = document.getElementById("menu-responsive");
        if (menu.className === "menu") {
                menu.className += " responsive";
        } else {
                menu.className = "menu";
        }
}