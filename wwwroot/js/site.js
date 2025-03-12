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

document.querySelectorAll(".tag-select").forEach(button => {
    button.addEventListener("click", function () {
        if (this.classList.contains("selected")) {
            document.querySelectorAll(".tag-select").forEach(btn => btn.classList.remove("selected"));
            document.getElementById("tagInput").name = ""
            document.getElementById("tagInput").value = "";
        }else {
            document.querySelectorAll(".tag-select").forEach(btn => btn.classList.remove("selected"));
            this.classList.add("selected");
            document.getElementById("tagInput").name = "Tags[0].Name"
            document.getElementById("tagInput").value = this.getAttribute("data-tag");
        }
    });
});

function addQuestion(button) {
    let wrapper = document.getElementById("question-wrapper");
    let count = wrapper.getElementsByClassName("question-container").length;

    button.setAttribute('onclick', 'addQuestion(this)');

    const container = document.createElement("div");
    container.classList.add("question-container");

    const input = document.createElement("input");
    input.type = "text";
    input.classList.add("question-input");
    input.name = `Questions[${count}].Content`;
    input.required = true;

    const newButton = document.createElement("button");
    newButton.classList.add("question-more");
    newButton.innerText = "-";
    newButton.type = "button";
    newButton.setAttribute('onclick', 'removeQuestion(this)');

    container.appendChild(input);
    container.appendChild(newButton);

    wrapper.appendChild(container);
}

function removeQuestion(button) {
    button.parentElement.remove();

    let inputs = document.querySelectorAll("#question-wrapper .question-input");
    inputs.forEach((input, index) => {
        input.name = `Questions[${index}].Content`;
    });
}

function showToast(message) {
    var toast = document.getElementById("toast");
    var toastMessage = document.getElementById("toast-message");

    toastMessage.textContent = message;
    toast.classList.remove("hide");
    toast.classList.add("show");

    setTimeout(function () {
        toast.classList.remove("show");
        toast.classList.add("hide");
    }, 3000);
}

document.addEventListener("DOMContentLoaded", function () {
    var toastElement = document.getElementById("toast");
    if (toastElement && toastElement.dataset.message) {
        showToast(toastElement.dataset.message);
    }
});

document.querySelectorAll('.tag-button').forEach(button => {
    button.addEventListener('click', function() {
        if (this.classList.contains('selected')) {
            document.getElementById('selectedTag').value = "";
        }else{
            document.getElementById('selectedTag').value = this.dataset.tag;
        }
        document.getElementById('filterForm').submit();
    });
});