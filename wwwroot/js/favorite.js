document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll(".fav-btn").forEach(button => {
            button.addEventListener("click", async function () {
                const postId = this.dataset.postId;
    
                try {
                    const response = await fetch(`/Post/Favorite/${postId}`, {
                        method: "POST",
                        headers: { "Content-Type": "application/json" }
                    });
    
                    if (!response.ok) throw new Error("Failed to update favorite.");
    
                    const data = await response.json();
                    this.dataset.isFav = data.isFav ? "True" : "False";
    
                    // เปลี่ยนสีหัวใจ
                    const svg = this.querySelector("svg");
                    svg.setAttribute("fill", data.isFav ? "#FF8A80" : "none");
                } catch (error) {
                    console.error(error);
                    alert("Error: Unable to update favorite status.");
                }
            });
        });
    });
    