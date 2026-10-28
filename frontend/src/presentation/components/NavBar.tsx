import logo from "../../assets/logo.svg";
import Nav from "react-bootstrap/Nav";
import Navbar from "react-bootstrap/Navbar";
import NavDropdown from "react-bootstrap/NavDropdown";
import Container from "react-bootstrap/Container";
import { useContext, useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";
import { AddRecipeModal } from "./AddRecipeModal";
import { ImportRecipeModal } from "./ImportRecipeModal";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import type { Category } from "../../types/Category";
import { organizeCategories } from "../../utils/categoryHelper";
import apiClient from "../../api/apiClient";

export default function NavBar() {
  const navigate = useNavigate();
  const { isLoggedIn, username, logout } = useContext(AuthContext);
  const [showAddRecipeModal, setShowAddRecipeModal] = useState(false);
  const [showImportRecipeModal, setShowImportRecipeModal] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loadingCategories, setLoadingCategories] = useState(false);

  useEffect(() => {
    const fetchCategories = async () => {
      try {
        setLoadingCategories(true);
        const response = await apiClient.get<Category[]>("/categories");
        setCategories(response.data);
      } catch (error) {
        console.error("Failed to fetch categories:", error);
      } finally {
        setLoadingCategories(false);
      }
    };

    fetchCategories();
  }, []);

  const handleShowAddRecipeModal = () => setShowAddRecipeModal(true);
  const handleCloseAddRecipeModal = () => setShowAddRecipeModal(false);
  const handleRecipeAdded = () => {
    toast.success("Recipe added successfully!");
    navigate("/recipes", { replace: true });
  };

  const handleShowImportRecipeModal = () => setShowImportRecipeModal(true);
  const handleCloseImportRecipeModal = () => setShowImportRecipeModal(false);
  const handleRecipeImported = () => {
    toast.success("Recipe imported successfully!");
    navigate("/recipes", { replace: true });
  };

  const hierarchicalCategories = organizeCategories(categories);

  const renderCategoryMenu = (hierarchicalCats: typeof hierarchicalCategories) => {
    return hierarchicalCats.map((mainCat) => (
      <NavDropdown
        key={mainCat.id}
        title={mainCat.name}
        id={`category-${mainCat.id}`}
        className="ps-1"
      >
        {mainCat.subcategories.length > 0 ? (
          <>
            {mainCat.subcategories.map((subCat) => (
              <NavDropdown.Item
                key={subCat.id}
                href={`/recipes?category=${subCat.id}`}
                className="ps-4"
              >
                {subCat.name}
              </NavDropdown.Item>
            ))}
            <NavDropdown.Divider />
          </>
        ) : null}
        <NavDropdown.Item
          href={`/recipes?category=${mainCat.id}`}
          className="fw-semibold"
        >
          All {mainCat.name}
        </NavDropdown.Item>
      </NavDropdown>
    ));
  };

  return (
    <div>
      <ToastContainer
        position="top-right"
        autoClose={5000}
        hideProgressBar={false}
        newestOnTop={false}
        closeOnClick
        rtl={false}
        pauseOnFocusLoss
        draggable
        pauseOnHover
      />
      <Navbar expand="lg" className="bg-body-tertiary">
        <Container>
          <Navbar.Brand href="/">
            <img src={logo} alt="Recipe Manager" height="30" />
          </Navbar.Brand>

          <Navbar.Toggle aria-controls="basic-navbar-nav" />

          <Navbar.Collapse id="basic-navbar-nav">
            <Nav className="me-auto">
              <Nav.Link href="/">Home</Nav.Link>

              <NavDropdown title="Recipes" id="recipes-nav-dropdown">
                <NavDropdown.Item href="/recipes">All Recipes</NavDropdown.Item>
                <NavDropdown.Item href="/recipes?show=my">
                  My Recipes
                </NavDropdown.Item>
                <NavDropdown.Divider />
                <NavDropdown.Item onClick={handleShowAddRecipeModal}>
                  Add Recipe
                </NavDropdown.Item>
                <NavDropdown.Item onClick={handleShowImportRecipeModal}>
                  Import Recipe From URL
                </NavDropdown.Item>
                <NavDropdown.Divider />
                <NavDropdown.Item href="/categories">
                  Manage Categories
                </NavDropdown.Item>
              </NavDropdown>

              {/* CATEGORIES MENU - Hierarchical */}
              {!loadingCategories && hierarchicalCategories.length > 0 && (
                <NavDropdown title="Categories" id="categories-nav-dropdown">
                  {renderCategoryMenu(hierarchicalCategories)}
                </NavDropdown>
              )}

              {/* USER MENU */}
              {!isLoggedIn ? (
                <Nav.Link href="/login">Login</Nav.Link>
              ) : (
                <NavDropdown
                  title={username || "Account"}
                  id="account-nav-dropdown"
                >
                  <NavDropdown.Item disabled>
                    Logged in as <strong>{username}</strong>
                  </NavDropdown.Item>
                  <NavDropdown.Divider />
                  <NavDropdown.Item onClick={logout}>Logout</NavDropdown.Item>
                </NavDropdown>
              )}
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>
      <AddRecipeModal
        show={showAddRecipeModal}
        onHide={handleCloseAddRecipeModal}
        onSuccess={handleRecipeAdded}
      />
      <ImportRecipeModal
        show={showImportRecipeModal}
        onHide={handleCloseImportRecipeModal}
        onSuccess={handleRecipeImported}
      />
    </div>
  );
}
