// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// function login() {
//         document.getElementById("login-text").textContent="incorrect username or password"
// }

function strcmp(a, b)
{   
    return (a<b?-1:(a>b?1:0));  
}

function signup() {
        const password = document.getElementById("password").value;
        const confirm_password = document.getElementById("confirm-password").value;

        if (password === confirm_password) {
                document.getElementById("error-text").textContent = ""
                document.getElementById("form").submit()
        }
        else {
                document.getElementById("error-text").textContent = "unmatch password"
        }

}

document.querySelectorAll(".tag").forEach(button => {
    button.addEventListener("click", function () {
        document.querySelectorAll(".tag").forEach(btn => btn.classList.remove("selected"));

        this.classList.add("selected");

        document.getElementById("tagInput").value = this.getAttribute("data-tag");
    });
});

function addQuestion(button) {
    // ลบปุ่มเก่าออก
    button.remove();

    // สร้างกล่อง input ใหม่
    const container = document.createElement("div");
    container.classList.add("question-container");
    container.style.marginTop = "20px"

    const input = document.createElement("input");
    input.type = "text";
    input.classList.add("question-input");
    input.name = "questions[]"; // ส่งเป็น array ไป backend

    const newButton = document.createElement("button");
    newButton.classList.add("question-more");
    newButton.innerText = "+";
    newButton.onclick = function () { addQuestion(newButton); };

    container.appendChild(input);
    container.appendChild(newButton);

    // เพิ่มกล่องใหม่ลงใน wrapper
    document.getElementById("question-wrapper").appendChild(container);
    console.log("done")
}