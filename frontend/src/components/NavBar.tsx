import logo from "../assets/logo.svg";
import Nav from "react-bootstrap/Nav";
import Navbar from "react-bootstrap/Navbar";
import NavDropdown from "react-bootstrap/NavDropdown";
import Container from "react-bootstrap/Container";
import { useContext } from "react";
import { AuthContext } from "./AuthContext";

export default function NavBar() {
  const { isLoggedIn, username, logout } = useContext(AuthContext);

  return (
    <div>
      <Navbar expand="lg" className="bg-body-tertiary">
        <Container>
          <Navbar.Brand href="/">
            <img src={logo} alt="Dishwise" height="30" />
          </Navbar.Brand>

          <Navbar.Toggle aria-controls="basic-navbar-nav" />

          <Navbar.Collapse id="basic-navbar-nav">
            <Nav className="me-auto">
              <Nav.Link href="/">Home</Nav.Link>

              <NavDropdown title="Recipes" id="recipes-nav-dropdown">
                <NavDropdown.Item href="/recipes">List</NavDropdown.Item>
                <NavDropdown.Item href="/recipes/addrecipe">
                  Add Recipe
                </NavDropdown.Item>
                <NavDropdown.Item href="/recipesimport">
                  Import Recipe From URL
                </NavDropdown.Item>
              </NavDropdown>

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
    </div>
  );
}
